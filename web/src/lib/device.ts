const DEVICE_KEY = 'cinewave.device';
const PROFILE_KEY = 'cinewave.profile';

export const DEVICE_NAMES = ['phone', 'TV', 'laptop'] as const;
export type DeviceName = (typeof DEVICE_NAMES)[number];

/** One random id per browser, combined with the chosen device name so a single browser can act as "another device" in the demo. */
function browserId(): string {
  let id = localStorage.getItem(`${DEVICE_KEY}.id`);
  if (!id) {
    id = Math.random().toString(36).slice(2, 10);
    localStorage.setItem(`${DEVICE_KEY}.id`, id);
  }
  return id;
}

export function currentDevice(): { id: string; name: DeviceName } {
  const stored = localStorage.getItem(`${DEVICE_KEY}.name`) as DeviceName | null;
  const name = stored && DEVICE_NAMES.includes(stored) ? stored : 'laptop';
  return { id: `${name.toLowerCase()}-${browserId()}`, name };
}

export function setDeviceName(name: DeviceName) { localStorage.setItem(`${DEVICE_KEY}.name`, name); }
export function storedProfileId(): string | null { return localStorage.getItem(PROFILE_KEY); }
export function storeProfileId(id: string) { localStorage.setItem(PROFILE_KEY, id); }
