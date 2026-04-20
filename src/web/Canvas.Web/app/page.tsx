"use client";

import { useEffect, useState } from "react";
import { fetchHealth } from "@/lib/api";

export default function Home() {
  const [status, setStatus] = useState<string>("checking...");
  const [healthy, setHealthy] = useState<boolean | null>(null);

  useEffect(() => {
    fetchHealth()
      .then((text) => {
        setStatus(text.trim());
        setHealthy(true);
      })
      .catch(() => {
        setStatus("unreachable");
        setHealthy(false);
      });
  }, []);

  return (
    <main className="min-h-screen flex flex-col items-center justify-center bg-gray-50">
      <div className="bg-white rounded-lg shadow p-10 text-center space-y-4 max-w-sm w-full">
        <h1 className="text-3xl font-bold text-gray-900">Canvas</h1>
        <p className="text-sm text-gray-400 uppercase tracking-widest">
          API Health
        </p>
        <p
          data-testid="health-status"
          className={`text-xl font-semibold ${
            healthy === null
              ? "text-gray-400"
              : healthy
              ? "text-green-600"
              : "text-red-500"
          }`}
        >
          {status}
        </p>
      </div>
    </main>
  );
}
