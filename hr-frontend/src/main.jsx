import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { addLocale, locale, localeOptions, PrimeReactProvider } from 'primereact/api';

import 'primereact/resources/themes/lara-light-indigo/theme.css';
import 'primereact/resources/primereact.min.css';
import 'primeicons/primeicons.css';
import './index.css'
import App from './App.jsx'

addLocale('es', {
  clear: 'Limpiar',
  apply: 'Aplicar',
  close: 'Cerrar',
  aria: {
    ...localeOptions('en').aria,
    close: 'Cerrar',
    filter: 'Filtrar',
    pageLabel: 'Página',
    firstPageLabel: 'Primera página',
    lastPageLabel: 'Última página',
    nextPageLabel: 'Página siguiente',
    previousPageLabel: 'Página anterior',
  },
});
locale('es');

createRoot(document.getElementById('root')).render(
  <StrictMode>
        <PrimeReactProvider>
            <App />
        </PrimeReactProvider>
    </StrictMode>,
)
