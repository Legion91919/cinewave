import { navigate } from '../lib/router';
import { Notice } from './Notice';

/** Placeholder for the hosted payment page hand-off (slice 3). Card data never touches Cinewave. */
export function RenewPage() {
  const plan = new URLSearchParams(window.location.search).get('plan') ?? 'standard';
  return (
    <main>
      <Notice kind="empty" title="Renew your subscription" message={`You'd now be sent to our payment partner's secure page to renew the ${plan} plan. That hand-off arrives in a later release.`}
        action={<button className="btn" onClick={() => navigate('/')}>Back to browse</button>} />
    </main>
  );
}
