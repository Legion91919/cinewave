import type { Profile } from '../api/types';
import { DEVICE_NAMES, type DeviceName } from '../lib/device';
import { navigate } from '../lib/router';
import { Logo } from './Logo';

const COUNTRY: Record<string, string> = { NG: 'Nigeria', ZA: 'South Africa', KE: 'Kenya', GH: 'Ghana', GB: 'the United Kingdom', SN: 'Senegal', US: 'the United States', FR: 'France' };
export const countryName = (code: string) => COUNTRY[code] ?? code;

interface Props {
  profiles: Profile[] | null;
  activeProfileId: string | null;
  deviceName: DeviceName;
  onSwitchProfile: (id: string) => void;
  onSwitchDevice: (name: DeviceName) => void;
}

export function TopBar({ profiles, activeProfileId, deviceName, onSwitchProfile, onSwitchDevice }: Props) {
  const active = profiles?.find((p) => p.id === activeProfileId);
  return (
    <header className="topbar">
      <a className="logo" href="/" onClick={(e) => { e.preventDefault(); navigate('/'); }}><Logo />Cinewave</a>
      <div className="who">
        {profiles === null ? <span className="skel" style={{ width: 160, height: 28 }} aria-label="Loading profiles" />
          : active ? <span className="territory">Films licensed in {countryName(active.territory)}</span>
          : <span className="territory">Territory unknown</span>}
        <label><span className="sr-only">Watching on</span>
          <select value={deviceName} onChange={(e) => onSwitchDevice(e.target.value as DeviceName)} aria-label="Device">
            {DEVICE_NAMES.map((d) => <option key={d} value={d}>On {d}</option>)}
          </select>
        </label>
        <label><span className="sr-only">Profile</span>
          <select value={activeProfileId ?? ''} onChange={(e) => onSwitchProfile(e.target.value)} aria-label="Profile" disabled={!profiles?.length}>
            {profiles?.length === 0 && <option value="">No profiles on this account</option>}
            {profiles?.map((p) => <option key={p.id} value={p.id}>{p.displayName} ({p.territory})</option>)}
          </select>
        </label>
      </div>
    </header>
  );
}
