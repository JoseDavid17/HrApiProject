import { useEffect, useRef, useState } from 'react';
import { Avatar } from 'primereact/avatar';
import { Button } from 'primereact/button';
import { Sidebar } from 'primereact/sidebar';
import HrFooter from './HrFooter';
import './HrLayout.css';

const mobileQuery = '(max-width: 1023px)';
const panelId = 'hr-sidebar-panel';

const groups = [
  {
    label: 'Navegación',
    items: [
      { label: 'General', icon: 'home' },
      { label: 'Bandeja de entrada', icon: 'inbox', badge: '12' },
      { label: 'Buscar', icon: 'search' },
      { label: 'Notificaciones', icon: 'bell', badge: '3' },
    ],
  },
  {
    label: 'Recursos humanos',
    action: true,
    items: [
      { label: 'Indicadores', icon: 'chart-bar', children: ['Resumen', 'Reportes', 'Actividad reciente'] },
      { label: 'Empleados', icon: 'id-card', children: [{ label: 'Lista de empleados', path: '/empleados' }] },
      { label: 'Departamentos', icon: 'building', children: [{ label: 'Lista de departamentos', path: '/departamentos' }] },
      { label: 'Regiones', icon: 'globe', children: [{ label: 'Lista de regiones', path: '/regiones' }] },
      { label: 'Países', icon: 'flag', children: [{ label: 'Lista de países', path: '/paises' }] },
      { label: 'Locaciones', icon: 'map-marker', children: [{ label: 'Lista de locaciones', path: '/locaciones' }] },
      { label: 'Equipo', icon: 'users' },
      { label: 'Calendario', icon: 'calendar' },
      { label: 'Documentos', icon: 'folder', children: ['Compartidos', 'Privados', 'Archivados'] },
    ],
  },
  {
    label: 'Nómina',
    items: [
      { label: 'Pagos', icon: 'credit-card' },
      { label: 'Órdenes de pago', icon: 'file' },
      { label: 'Beneficios', icon: 'star' },
    ],
  },
];

function Icon({ name, className = '' }) {
  return <i className={`pi pi-${name} ${className}`} aria-hidden="true" />;
}

function HrSidebarMenu({ collapsed, activeItems, activePath, openGroups, onSelect, onNavigate, onToggleGroup }) {
  return (
    <div className="hr-panel" data-collapsed={collapsed}>
      <div className="hr-panel__header">
        <div className="hr-identity" title={collapsed ? 'Recursos Humanos' : undefined}>
          <span className="hr-logo" aria-hidden="true">HR</span>
          <span className="hr-label hr-brand">Recursos Humanos</span>
        </div>
      </div>

      <nav className="hr-navigation" aria-label="Navegación de recursos humanos">
        {groups.map(({ label, items, action }) => (
          <div className="hr-group" key={label}>
            <div className="hr-group__heading">
              <span>{label}</span>
              {action && <Icon name="plus" />}
            </div>

            <ul className="hr-list">
              {items.map(({ label, icon, badge, children }) => {
                const hasActiveChild = children?.some((child) => child.path === activePath);
                const expanded = Boolean(children && (openGroups[label] ?? hasActiveChild) && !collapsed);
                const submenuId = `hr-submenu-${label.toLowerCase()}`;
                const isActive = activeItems.includes(label);

                return (
                  <li className="hr-item" key={label}>
                    <button
                      type="button"
                      className="hr-row"
                      aria-current={isActive ? 'true' : undefined}
                      aria-expanded={children ? expanded : undefined}
                      aria-controls={children ? submenuId : undefined}
                      title={collapsed ? label : undefined}
                      onClick={() => children ? onToggleGroup(label, expanded) : onSelect(label)}
                    >
                      <Icon name={icon} />
                      <span className="hr-label">{label}</span>
                      {children && <Icon name="chevron-down" className="hr-chevron" />}
                      {badge && <span className="hr-badge">{badge}</span>}
                      {!children && !badge && <Icon name="ellipsis-v" className="hr-action" />}
                    </button>
                    {children && (
                      <ul className="hr-submenu" id={submenuId} hidden={!expanded}>
                        {children.map((child) => child.path ? (
                          <li key={child.path}>
                            <a className="hr-subrow" href={`#${child.path}`}
                              aria-current={child.path === activePath ? 'page' : undefined}
                              onClick={onNavigate}>
                              {child.label}
                            </a>
                          </li>
                        ) : (
                          <li key={child}>
                            <button type="button" className="hr-subrow"
                              aria-current={activeItems.includes(child) ? 'true' : undefined}
                              onClick={() => onSelect(child)}>
                              {child}
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>

      <footer className="hr-panel__footer">
        <div className="hr-identity" title={collapsed ? 'Usuario de RR. HH.' : undefined}>
          <Avatar className="hr-avatar" icon="pi pi-user" shape="circle" aria-hidden="true" />
          <span className="hr-label">Usuario de RR. HH.</span>
        </div>
      </footer>
    </div>
  );
}

export default function HrLayout({ activePath, children, onSignOut }) {
  const [isMobile, setIsMobile] = useState(() => window.matchMedia(mobileQuery).matches);
  const [collapsed, setCollapsed] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerActive, setDrawerActive] = useState(false);
  const [activeItems, setActiveItems] = useState(['General', 'Resumen']);
  const [openGroups, setOpenGroups] = useState({ Indicadores: true });
  const triggerRef = useRef(null);

  useEffect(() => {
    const media = window.matchMedia(mobileQuery);

    function handleResize({ matches }) {
      setIsMobile(matches);
      setDrawerOpen(false);
      setDrawerActive(false);
      setCollapsed(false);
    }

    media.addEventListener('change', handleResize);
    return () => media.removeEventListener('change', handleResize);
  }, []);

  function toggleSidebar() {
    if (isMobile) {
      setDrawerOpen(true);
      setDrawerActive(true);
      return;
    }

    setCollapsed((value) => !value);
  }

  function selectItem(label) {
    setActiveItems([label]);
    setDrawerOpen(false);
    if (activePath !== '/') window.location.hash = '/';
  }

  function toggleGroup(label, expanded) {
    setOpenGroups((current) => ({ ...current, [label]: !expanded }));
    setCollapsed(false);
  }

  function restoreFocus() {
    setDrawerActive(false);
    requestAnimationFrame(() => triggerRef.current?.focus());
  }

  const menu = (
    <HrSidebarMenu
      collapsed={!isMobile && collapsed}
      activeItems={activePath === '/' ? activeItems : []}
      activePath={activePath}
      openGroups={openGroups}
      onSelect={selectItem}
      onNavigate={() => setDrawerOpen(false)}
      onToggleGroup={toggleGroup}
    />
  );

  return (
    <div className="hr-layout">
      {isMobile ? (
        <Sidebar
          id={panelId}
          visible={drawerOpen}
          className="hr-drawer"
          maskClassName="hr-drawer-mask"
          appendTo="self"
          modal
          blockScroll
          onHide={() => setDrawerOpen(false)}
          transitionOptions={{ onExited: restoreFocus }}
          aria-label="Menú lateral"
          ariaCloseLabel="Cerrar menú lateral"
        >
          {menu}
        </Sidebar>
      ) : (
        <aside id={panelId} className="hr-aside" data-collapsed={collapsed}>
          {menu}
          <button
            className="hr-rail"
            type="button"
            tabIndex={-1}
            aria-label="Alternar menú lateral"
            onClick={toggleSidebar}
          />
        </aside>
      )}

      <div className="hr-workspace" inert={isMobile && drawerActive}>
        <header className="hr-topbar">
          <Button
            ref={triggerRef}
            type="button"
            icon="pi pi-sidebar"
            text
            severity="secondary"
            className="hr-trigger"
            aria-label="Alternar menú lateral"
            aria-expanded={isMobile ? drawerOpen : !collapsed}
            aria-controls={!isMobile || drawerActive ? panelId : undefined}
            onClick={toggleSidebar}
          />
          <Button type="button" icon="pi pi-sign-out" label="Cerrar sesión"
            text severity="secondary" className="hr-sign-out" onClick={onSignOut} />
        </header>

        <main className="hr-content">
          {children}
        </main>
        <HrFooter />
      </div>
    </div>
  );
}

