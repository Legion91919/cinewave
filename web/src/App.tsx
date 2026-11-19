import { lazy, Suspense, useEffect, useState } from 'react';
import { api } from './api/client';
import type { Profile } from './api/types';
import { BrowsePage } from './components/BrowsePage';
import { Notice } from './components/Notice';
import { RenewPage } from './components/RenewPage';
import { TitleDetail } from './components/TitleDetail';
import { TopBar } from './components/TopBar';
import { currentDevice, setDeviceName, storedProfileId, storeProfileId, type DeviceName } from './lib/device';
import { navigate, useRoute } from './lib/router';

// hls.js only loads when someone presses Play, keeping the browse screen light.
const Player = lazy(() => import('./components/Player').then((m) => ({ default: m.Player })));

export function App() {
  const route = useRoute();
  const [profiles, setProfiles] = useState<Profile[] | null>(null);
  const [profilesError, setProfilesError] = useState<string | null>(null);
  const [profileId, setProfileId] = useState<string | null>(storedProfileId());
  const [device, setDevice] = useState(currentDevice());

  useEffect(() => {
    api.profiles().then((list) => {
      setProfiles(list);
      if (!list.some((p) => p.id === profileId) && list[0]) setProfileId(list[0].id);
    }, (e: Error) => { setProfiles([]); setProfilesError(e.message); });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const switchProfile = (id: string) => { storeProfileId(id); setProfileId(id); navigate('/'); };
  const switchDevice = (name: DeviceName) => { setDeviceName(name); setDevice(currentDevice()); };

  let body: JSX.Element;
  if (route.name === 'renew') body = <RenewPage />;
  else if (profilesError) body = <main><Notice kind="error" title="We couldn't reach Cinewave" message={profilesError} action={<button className="btn primary" onClick={() => window.location.reload()}>Try again</button>} /></main>;
  else if (!profileId || profiles === null) body = <main aria-busy="true"><section className="hero"><div className="skel" style={{ width: 420, maxWidth: '80%', height: 48 }} /></section></main>;
  else if (route.name === 'title') body = <TitleDetail profileId={profileId} titleId={route.titleId} />;
  else if (route.name === 'watch') body = <Suspense fallback={<div className="stage"><div className="spinner" role="progressbar" aria-label="Loading player" /></div>}><Player key={`${profileId}-${route.titleId}-${device.id}`} profileId={profileId} titleId={route.titleId} device={device} onExit={() => navigate('/')} /></Suspense>;
  else body = <BrowsePage key={`${profileId}-${device.id}`} profileId={profileId} />;

  return (
    <>
      {route.name !== 'watch' && <TopBar profiles={profiles} activeProfileId={profileId} deviceName={device.name} onSwitchProfile={switchProfile} onSwitchDevice={switchDevice} />}
      {body}
    </>
  );
}
