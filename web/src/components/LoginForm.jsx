import { useState } from "react";
import { api } from "../api";

export default function LoginForm({ onSuccess, onSwitchToRegister }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const user = await api.post("/auth/login", { email, password });
      onSuccess(user);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form className="auth-form" onSubmit={handleSubmit}>
      <h3>Giriş yap</h3>
      <input
        type="email"
        placeholder="E-posta"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        autoComplete="email"
        required
      />
      <input
        type="password"
        placeholder="Şifre"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        autoComplete="current-password"
        required
      />
      {error && <p className="auth-error">{error}</p>}
      <button className="btn-primary" type="submit" disabled={loading}>
        {loading ? "Giriş yapılıyor..." : "Giriş yap"}
      </button>
      <button type="button" className="link-button" onClick={onSwitchToRegister}>
        Hesabın yok mu? Kayıt ol
      </button>
    </form>
  );
}
