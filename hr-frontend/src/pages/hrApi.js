import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_HR_API_URL || 'https://localhost:7078/api',
});

const sessionKey = 'hr-api-token';
const sessionChange = 'hr-session-change';
const pageSize = 50;

export function sessionExpiresAt(token) {
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const { exp } = JSON.parse(atob(payload));
    return Number.isFinite(exp) ? exp * 1000 : 0;
  } catch {
    return 0;
  }
}

export function getSessionToken() {
  const token = sessionStorage.getItem(sessionKey);
  return token && sessionExpiresAt(token) > Date.now() ? token : null;
}

export function subscribeToSession(onChange) {
  window.addEventListener(sessionChange, onChange);
  return () => window.removeEventListener(sessionChange, onChange);
}

export function signOut() {
  sessionStorage.removeItem(sessionKey);
  window.dispatchEvent(new Event(sessionChange));
}

export async function signIn(credentials) {
  const { data } = await api.post('/Auth/login', credentials);
  if (typeof data?.token !== 'string' || sessionExpiresAt(data.token) <= Date.now()) {
    throw new Error('La API no devolvió un token de sesión válido.');
  }
  sessionStorage.setItem(sessionKey, data.token);
  window.dispatchEvent(new Event(sessionChange));
}

api.interceptors.request.use((request) => {
  if (request.url !== '/Auth/login') {
    const token = getSessionToken();
    if (token) request.headers.set('Authorization', `Bearer ${token}`);
  }
  return request;
});

api.interceptors.response.use(undefined, (error) => {
  const currentToken = sessionStorage.getItem(sessionKey);
  if (error.response?.status === 401 && currentToken &&
      error.config?.headers?.get('Authorization') === `Bearer ${currentToken}`) {
    signOut();
  }
  return Promise.reject(error);
});

export async function listRecords({ endpoint, paginated }, signal) {
  const records = [];
  let pageNumber = 1;
  let page;

  do {
    const { data } = await api.get(`/${endpoint}`, {
      signal,
      params: paginated ? { pageNumber, pageSize } : undefined,
    });
    if (!Array.isArray(data)) throw new Error('La API no devolvió una lista de registros.');
    page = data;
    records.push(...page);
    pageNumber += 1;
  } while (paginated && page.length === pageSize);

  return records;
}

export async function createRecord(endpoint, values) {
  await api.post(`/${endpoint}`, values);
}

export async function updateRecord(endpoint, id, values, method = 'PATCH') {
  await api.request({ method, url: `/${endpoint}/${encodeURIComponent(id)}`, data: values });
}

export async function deleteRecord(endpoint, id) {
  await api.delete(`/${endpoint}/${encodeURIComponent(id)}`);
}

export async function changeDepartmentManager(departmentId, managerId) {
  await api.patch(`/Departments/ChangeManager/${encodeURIComponent(departmentId)}`, managerId, {
    headers: { 'Content-Type': 'application/json' },
  });
}

export function apiErrorMessage(error, fallback) {
  const status = error.response?.status;
  if (status === 403) return 'Tu cuenta no tiene permisos para realizar esta operación.';
  const data = error.response?.data;
  const messages = Object.values(data?.errors ?? {}).flat()
    .filter((message) => typeof message === 'string' && message.trim());
  if (messages.length) return [...new Set(messages)].join('\n');

  return [data?.detail, data?.message, data?.title]
    .find((message) => typeof message === 'string' && message.trim()) ?? fallback;
}
