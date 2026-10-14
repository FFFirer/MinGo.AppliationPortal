// Minimal service worker: satisfies Chromium installability (fetch handler required).
// Intentionally does NOT cache anything — L2 offline/cache is out of scope.
self.addEventListener('install', function () {
  self.skipWaiting();
});

self.addEventListener('activate', function (event) {
  event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', function () {
  // Pass through — no interception, no cache.
});
