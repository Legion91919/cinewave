import type { ReactNode } from 'react';

export function Notice({ kind, title, message, action }: { kind: 'empty' | 'error'; title: string; message: string; action?: ReactNode }) {
  return (
    <div className={`notice ${kind}`} role={kind === 'error' ? 'alert' : 'status'}>
      <h1>{title}</h1>
      <p className="meta">{message}</p>
      {action}
    </div>
  );
}
