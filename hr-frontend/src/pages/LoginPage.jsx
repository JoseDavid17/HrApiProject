import { useState } from 'react';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import HrFooter from '../layout/HrFooter';
import { apiErrorMessage, signIn } from './hrApi';
import './LoginPage.css';

export default function LoginPage({ notify }) {
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();
    if (busy) return;

    const form = new FormData(event.currentTarget);
    const username = String(form.get('username') ?? '').trim();
    const password = String(form.get('password') ?? '');
    if (!username || !password) {
      notify('warn', 'Campos obligatorios', 'Ingresa tu usuario y contraseña.');
      return;
    }

    setBusy(true);
    try {
      await signIn({ username, password });
    } catch (requestError) {
      notify('error', 'No se pudo iniciar sesión', requestError.response?.status === 401
        ? 'Usuario o contraseña incorrectos.'
        : apiErrorMessage(requestError, 'No se pudo iniciar sesión. Inténtalo de nuevo.'));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="hr-login">
      <header className="hr-login__header">
        <div className="hr-identity">
          <span className="hr-logo" aria-hidden="true">HR</span>
          <span className="hr-brand">Recursos Humanos</span>
        </div>
      </header>
      <main className="hr-login__main">
        <section className="hr-login__card" aria-labelledby="login-title">
          <h1 id="login-title">Iniciar sesión</h1>
          <p className="hr-login__description">Ingresa tus credenciales para acceder al sistema de Recursos Humanos.</p>

          <form className="hr-login__form" noValidate onSubmit={handleSubmit} aria-busy={busy}>
            <div className="hr-login__field">
              <label htmlFor="login-username">Usuario</label>
              <InputText id="login-username" name="username" autoComplete="username"
                autoCapitalize="none" spellCheck={false} required readOnly={busy} />
            </div>
            <div className="hr-login__field">
              <label htmlFor="login-password">Contraseña</label>
              <div className="hr-login__password">
                <InputText id="login-password" name="password" type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password" required readOnly={busy} />
                <button type="button" className="hr-login__visibility"
                  aria-label={showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                  onClick={() => setShowPassword((visible) => !visible)}>
                  <i className={showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'} aria-hidden="true" />
                </button>
              </div>
            </div>
            <Button type="submit" label={busy ? 'Ingresando…' : 'Iniciar sesión'}
              loading={busy} className="hr-login__submit" />
          </form>
        </section>
      </main>
      <HrFooter />
    </div>
  );
}
