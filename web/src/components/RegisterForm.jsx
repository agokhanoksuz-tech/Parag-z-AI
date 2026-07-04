import { useState } from "react";
import { api } from "../api";

export default function RegisterForm({ onSuccess, onSwitchToLogin }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const user = await api.post("/auth/register", { email, password });
      onSuccess(user);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <form className="auth-form" onSubmit={handleSubmit}>
      <h3>Hesap oluştur</h3>
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
        placeholder="Şifre (en az 8 karakter)"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        autoComplete="new-password"
        minLength={8}
        required
      />
      {error && <p className="auth-error">{error}</p>}
      <button className="btn-primary" type="submit" disabled={loading}>
        {loading ? "Kaydediliyor..." : "Kayıt ol"}
      </button>
      <button type="button" className="link-button" onClick={onSwitchToLogin}>
        Zaten hesabın var mı? Giriş yap
      </button>
    </form>
  );
}
