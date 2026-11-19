import { ApiError, createClient, PaymentRequiredError } from './client';

const respond = (status: number, body: unknown) =>
  vi.fn().mockResolvedValue(new Response(JSON.stringify(body), { status, headers: { 'content-type': 'application/json' } }));

describe('api client', () => {
  it('turns a 402 from play into PaymentRequiredError with the renew link', async () => {
    const client = createClient(respond(402, { title: 'Subscription required', status: 402, detail: 'Your subscription ended.', renewUrl: '/account/renew?plan=standard' }));

    const err = await client.play('p-wanjiru', 'nightbus-to-bulawayo').catch((e) => e);

    expect(err).toBeInstanceOf(PaymentRequiredError);
    expect(err.renewUrl).toBe('/account/renew?plan=standard');
    expect(err.message).toBe('Your subscription ended.');
  });

  it('keeps 451 as a plain ApiError with its status', async () => {
    const client = createClient(respond(451, { title: 'Not available in your country', status: 451 }));

    const err = await client.title('p-adaeze', 'red-earth-choir').catch((e) => e);

    expect(err).toBeInstanceOf(ApiError);
    expect(err).not.toBeInstanceOf(PaymentRequiredError);
    expect(err.status).toBe(451);
  });

  it('sends progress as an idempotent PUT with the client timestamp', async () => {
    const fetchMock = respond(200, { applied: true, positionSeconds: 750 });
    const client = createClient(fetchMock);

    await client.saveProgress('p-adaeze', 'the-salt-road', { positionSeconds: 750, durationSeconds: 1200, deviceId: 'phone-x', deviceName: 'phone', clientTimestamp: '2026-11-19T19:00:00.000Z' });

    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('/api/profiles/p-adaeze/progress/the-salt-road');
    expect(init.method).toBe('PUT');
    expect(JSON.parse(init.body).clientTimestamp).toBe('2026-11-19T19:00:00.000Z');
  });
});
