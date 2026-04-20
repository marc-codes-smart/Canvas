const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";

export async function fetchHealth(): Promise<string> {
  const res = await fetch(`${apiUrl}/health`);
  if (!res.ok) throw new Error(`Health check returned ${res.status}`);
  return res.text();
}
