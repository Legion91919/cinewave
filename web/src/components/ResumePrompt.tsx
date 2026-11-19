import { formatTimecode } from '../lib/time';

export function ResumePrompt({ positionSeconds, stoppedOnDevice, onResume, onRestart }: {
  positionSeconds: number; stoppedOnDevice: string | null; onResume: () => void; onRestart: () => void;
}) {
  const at = formatTimecode(positionSeconds);
  return (
    <div className="overlay">
      <div className="dialog" role="dialog" aria-modal="true" aria-labelledby="resume-h">
        <h2 id="resume-h">Resume at {at}?</h2>
        {stoppedOnDevice && <p className="meta">You stopped here on your {stoppedOnDevice}.</p>}
        <div className="actions">
          <button className="btn primary" autoFocus onClick={onResume}>▶ Resume at {at}</button>
          <button className="btn" onClick={onRestart}>Start from the beginning</button>
        </div>
      </div>
    </div>
  );
}
