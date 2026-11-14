import { useCallback, useEffect, useRef, useSyncExternalStore } from 'react';
import { Toast } from 'primereact/toast';
import HrLayout from './layout/HrLayout';
import LoginPage from './pages/LoginPage';
import HrModulePage from './pages/HrModulePage';
import { hrModules } from './pages/hrModules';
import { getSessionToken, sessionExpiresAt, signOut, subscribeToSession } from './pages/hrApi';
import './components/HrToast.css';

const toastIcons = {
  success: 'pi pi-check',
  error: 'pi pi-times',
  warn: 'pi pi-exclamation-triangle',
  info: 'pi pi-info-circle',
};

function subscribeToLocation(onChange) {
  window.addEventListener('hashchange', onChange);
  return () => window.removeEventListener('hashchange', onChange);
}

function getCurrentPath() {
  return window.location.hash.slice(1) || '/';
}

function replacePath(path) {
  window.history.replaceState(window.history.state, '', `#${path}`);
  window.dispatchEvent(new Event('hashchange'));
}

export default function App() {
  const toast = useRef(null);
  const notify = useCallback((severity, summary, detail) => {
    toast.current?.show({ severity, summary, detail, icon: toastIcons[severity], life: severity === 'success' ? 3500 : 6000 });
  }, []);
  const location = useSyncExternalStore(subscribeToLocation, getCurrentPath);
  const token = useSyncExternalStore(subscribeToSession, getSessionToken);
  const [pathname, query] = location.split('?', 2);
  const isLogin = pathname === '/login';
  const next = isLogin ? new URLSearchParams(query).get('next') : null;
  const path = isLogin ? (Object.hasOwn(hrModules, next) ? next : '/') : location;
  const module = Object.hasOwn(hrModules, path) ? hrModules[path] : null;

  useEffect(() => {
    if (!token && !isLogin) {
      replacePath(module ? `/login?next=${encodeURIComponent(path)}` : '/login');
    } else if (token && isLogin) {
      replacePath(path);
    }
  }, [isLogin, module, path, token]);

  useEffect(() => {
    if (!token) return;
    const timer = window.setTimeout(signOut, Math.max(0, sessionExpiresAt(token) - Date.now()));
    return () => window.clearTimeout(timer);
  }, [token]);

  return (
    <>
      <Toast ref={toast} position="top-right" className="hr-toast" />
      {token ? (
        <HrLayout activePath={path} onSignOut={() => {
          signOut();
          replacePath('/login');
        }}>
          {module ? (
            <HrModulePage key={path} module={module} notify={notify} />
          ) : path === '/' ? (
            <>
              <div className="hr-placeholder" aria-hidden="true" />
              <div className="hr-placeholder" aria-hidden="true" />
            </>
          ) : (
            <section>
              <h1>Página no encontrada</h1>
              <a href="#/">Volver a General</a>
            </section>
          )}
        </HrLayout>
      ) : <LoginPage notify={notify} />}
    </>
  );
}
