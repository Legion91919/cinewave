import Hls from 'hls.js';
import { useCallback, useEffect, useRef, useState } from 'react';
import { api, PaymentRequiredError } from '../api/client';
import type { PlayResponse } from '../api/types';
import { shouldOfferResume, shouldReport } from '../lib/resume';
import { formatTimecode } from '../lib/time';
import { Notice } from './Notice';
import { PaywallNotice } from './PaywallNotice';
import { ResumePrompt } from './ResumePrompt';

type Phase =
  | { kind: 'loading' }
  | { kind: 'paywall'; renewUrl: string; detail: string }
  | { kind: 'error'; message: string }
  | { kind: 'prompt'; play: PlayResponse }
  | { kind: 'playing'; play: PlayResponse; startAt: number };

interface Props { profileId: string; titleId: string; device: { id: string; name: string }; onExit: () => void }

/**
 * Plays the HLS stream straight from the CDN URL the API returned (hls.js, falling back to native HLS on Safari/iOS).
 * Progress goes back to the API as small idempotent PUTs: every 10s of playback, on pause and on exit.
 */
export function Player({ profileId, titleId, device, onExit }: Props) {
  const [phase, setPhase] = useState<Phase>({ kind: 'loading' });
  const [attempt, setAttempt] = useState(0);
  const [saved, setSaved] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const lastReported = useRef<number | null>(null);

  useEffect(() => {
    let cancelled = false;
    setPhase({ kind: 'loading' });
    api.play(profileId, titleId).then(
      (play) => {
        if (cancelled) return;
        setPhase(shouldOfferResume(play.resumeAtSeconds, null) ? { kind: 'prompt', play } : { kind: 'playing', play, startAt: 0 });
      },
      (err: Error) => {
        if (cancelled) return;
        if (err instanceof PaymentRequiredError) setPhase({ kind: 'paywall', renewUrl: err.renewUrl, detail: err.message });
        else setPhase({ kind: 'error', message: err.message });
      },
    );
    return () => { cancelled = true; };
  }, [profileId, titleId, attempt]);

  const report = useCallback((force: boolean) => {
    const v = videoRef.current;
    if (!v || !Number.isFinite(v.duration) || v.duration <= 0) return;
    if (!shouldReport(lastReported.current, v.currentTime, force)) return;
    lastReported.current = v.currentTime;
    const position = v.currentTime;
    api.saveProgress(profileId, titleId, {
      positionSeconds: position, durationSeconds: v.duration, deviceId: device.id, deviceName: device.name, clientTimestamp: new Date().toISOString(),
    }).then(() => setSaved(formatTimecode(position)), () => setSaved(null));
  }, [profileId, titleId, device.id, device.name]);

  // Attach the stream once we know where to start.
  useEffect(() => {
    if (phase.kind !== 'playing') return;
    const video = videoRef.current;
    if (!video) return;
    const url = phase.play.manifestUrl;
    let hls: Hls | null = null;
    const start = () => { video.play().catch(() => { /* autoplay blocked: controls are visible */ }); };
    if (Hls.isSupported()) {
      hls = new Hls({ startPosition: phase.startAt });
      hls.on(Hls.Events.MANIFEST_PARSED, start);
      hls.on(Hls.Events.ERROR, (_e, data) => { if (data.fatal) setPhase({ kind: 'error', message: `Stream error: ${data.details}` }); });
      hls.loadSource(url);
      hls.attachMedia(video);
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
      video.src = url;
      video.addEventListener('loadedmetadata', () => { video.currentTime = phase.startAt; start(); }, { once: true });
    } else {
      setPhase({ kind: 'error', message: "This browser can't play adaptive (HLS) video." });
    }
    const onPageHide = () => report(true);
    window.addEventListener('pagehide', onPageHide);
    return () => {
      report(true);
      window.removeEventListener('pagehide', onPageHide);
      hls?.destroy();
    };
  }, [phase, report]);

  const title = phase.kind === 'playing' || phase.kind === 'prompt' ? phase.play.titleName : 'Starting…';
  return (
    <div className="stage" role="region" aria-label="Video player">
      {phase.kind === 'playing' && (
        <video ref={videoRef} controls playsInline aria-label="Film"
          onTimeUpdate={() => report(false)} onPause={() => report(true)} onEnded={() => report(true)} />
      )}
      <div className="top">
        <button className="btn icon" aria-label="Back to browse" onClick={onExit}>←</button>
        <h1>{title}</h1>
      </div>
      {phase.kind === 'playing' && saved && <span className="status" role="status">Progress saved at {saved}</span>}
      {phase.kind === 'loading' && <div className="overlay" aria-busy="true"><div className="spinner" role="progressbar" aria-label="Starting stream" /></div>}
      {phase.kind === 'prompt' && (
        <ResumePrompt positionSeconds={phase.play.resumeAtSeconds} stoppedOnDevice={phase.play.resumeFromDevice}
          onResume={() => setPhase({ kind: 'playing', play: phase.play, startAt: phase.play.resumeAtSeconds })}
          onRestart={() => setPhase({ kind: 'playing', play: phase.play, startAt: 0 })} />
      )}
      {phase.kind === 'paywall' && <PaywallNotice renewUrl={phase.renewUrl} detail={phase.detail} onBack={onExit} />}
      {phase.kind === 'error' && (
        <div className="overlay"><Notice kind="error" title="The stream didn't start" message={`${phase.message} Your place is saved.`}
          action={<div className="actions" style={{ justifyContent: 'center' }}><button className="btn primary" onClick={() => setAttempt((n) => n + 1)}>Try again</button><button className="btn" onClick={onExit}>Back to browse</button></div>} /></div>
      )}
    </div>
  );
}
