const BASE_URL = "/api";

async function request(path, { method = "GET", body } = {}) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    credentials: "include",
    headers: body !== undefined ? { "Content-Type": "application/json" } : undefined,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!res.ok) {
    let message = `İstek başarısız oldu (${res.status})`;
    try {
      const data = await res.json();
      if (data.message) {
        message = data.message;
      } else if (data.errors) {
        const firstKey = Object.keys(data.errors)[0];
        if (firstKey) message = data.errors[firstKey][0];
      }
    } catch {
      // yanıt JSON değilse orijinal mesaj kalır
    }
    throw new Error(message);
  }

  if (res.status === 204) return null;
  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: "POST", body: body ?? {} }),
  put: (path, body) => request(path, { method: "PUT", body: body ?? {} }),
  del: (path) => request(path, { method: "DELETE" }),
};
