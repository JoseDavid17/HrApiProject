import { useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { Avatar } from 'primereact/avatar';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import { OrganizationChart } from 'primereact/organizationchart';
import { Skeleton } from 'primereact/skeleton';
import { apiErrorMessage, listRecords } from './hrApi';
import './OrganizationChartPage.css';

function toChartNode(employee, expanded = false) {
  return {
    key: String(employee.employeeId),
    label: employee.fullName,
    data: employee.jobTitle,
    expanded,
    children: employee.children.map((child) => toChartNode(child)),
  };
}

function managerOptions(roots) {
  const options = [];
  const pending = [...roots];

  while (pending.length > 0) {
    const employee = pending.pop();
    if (employee.children.length === 0) continue;

    options.push({
      label: `${employee.fullName} · #${employee.employeeId}`,
      value: employee.employeeId,
    });
    pending.push(...employee.children);
  }

  return options.sort((first, second) => first.label.localeCompare(second.label, 'es-HN'));
}

function renderEmployee(node) {
  const initials = node.label.trim().split(/\s+/).slice(0, 2)
    .map((word) => word.charAt(0)).join('').toUpperCase();

  return (
    <div className="hr-org-chart__employee">
      <Avatar label={initials} shape="circle" aria-hidden="true" />
      <strong>{node.label}</strong>
      {node.data && <span className="hr-org-chart__job">{node.data}</span>}
    </div>
  );
}

const minZoom = 0.1;
const maxZoom = 2.5;
const zoomStep = 1.2;
const arrowOffsets = {
  ArrowLeft: [48, 0],
  ArrowRight: [-48, 0],
  ArrowUp: [0, 48],
  ArrowDown: [0, -48],
};

function zoomAt(camera, factor, x = 0, y = 0) {
  const scale = Math.max(Math.min(minZoom, camera.scale), Math.min(maxZoom, camera.scale * factor));
  const ratio = scale / camera.scale;
  return { scale, x: x + (camera.x - x) * ratio, y: y + (camera.y - y) * ratio };
}

function fitCamera(viewport, diagram, minimumScale = 0) {
  const scale = Math.max(minimumScale, Math.min(
    1,
    viewport.clientWidth / diagram.offsetWidth,
    viewport.clientHeight / diagram.offsetHeight,
  ));
  const y = Math.max(0, (diagram.offsetHeight * scale - viewport.clientHeight) / 2);
  return { scale, x: 0, y };
}

function OrganizationChartCanvas({ roots }) {
  const viewportRef = useRef(null);
  const diagramRef = useRef(null);
  const drag = useRef(null);
  const [camera, setCamera] = useState(null);
  const charts = useMemo(() => roots.map((root) => (
    <OrganizationChart key={root.key} value={[root]} nodeTemplate={renderEmployee} />
  )), [roots]);

  useLayoutEffect(() => {
    const viewport = viewportRef.current;
    const diagram = diagramRef.current;
    const frame = requestAnimationFrame(() => {
      setCamera(fitCamera(viewport, diagram, 0.9));
    });

    function handleWheel(event) {
      if (event.deltaY === 0 || event.ctrlKey || event.metaKey) return;
      event.preventDefault();
      const bounds = viewport.getBoundingClientRect();
      const x = event.clientX - bounds.left - bounds.width / 2;
      const y = event.clientY - bounds.top - bounds.height / 2;
      const factor = Math.exp(-Math.max(-100, Math.min(100, event.deltaY)) * 0.003);
      setCamera((current) => current && zoomAt(current, factor, x, y));
    }

    viewport.addEventListener('wheel', handleWheel, { passive: false });
    return () => {
      cancelAnimationFrame(frame);
      viewport.removeEventListener('wheel', handleWheel);
    };
  }, []);

  function moveCamera(x, y) {
    setCamera((current) => current && { ...current, x: current.x + x, y: current.y + y });
  }

  function startDrag(event) {
    if (!event.isPrimary || event.button !== 0 || event.target.closest('a, button')) return;
    drag.current = { id: event.pointerId, x: event.clientX, y: event.clientY };
    event.currentTarget.setPointerCapture(event.pointerId);
    event.currentTarget.dataset.dragging = 'true';
  }

  function dragCanvas(event) {
    const previous = drag.current;
    if (previous?.id !== event.pointerId) return;
    drag.current = { id: event.pointerId, x: event.clientX, y: event.clientY };
    moveCamera(event.clientX - previous.x, event.clientY - previous.y);
  }

  function stopDrag(event) {
    if (drag.current?.id !== event.pointerId) return;
    drag.current = null;
    delete event.currentTarget.dataset.dragging;
  }

  function keepBranchInPlace(event) {
    const toggle = event.target.closest('.p-node-toggler');
    if (!toggle) return;

    const before = toggle.getBoundingClientRect();
    requestAnimationFrame(() => {
      if (!toggle.isConnected) return;
      const after = toggle.getBoundingClientRect();
      moveCamera(before.left - after.left, before.top - after.top);
    });
  }

  function handleKeys(event) {
    if (event.target !== event.currentTarget || !Object.hasOwn(arrowOffsets, event.key)) return;
    event.preventDefault();
    moveCamera(...arrowOffsets[event.key]);
  }

  return (
    <div className="hr-org-chart__canvas">
      <div className="hr-org-chart__viewport" ref={viewportRef} role="region" tabIndex={0}
        aria-label="Jerarquía de empleados" aria-describedby="org-chart-navigation"
        onPointerDown={startDrag} onPointerMove={dragCanvas} onPointerUp={stopDrag}
        onPointerCancel={stopDrag} onLostPointerCapture={stopDrag}
        onClickCapture={keepBranchInPlace} onKeyDown={handleKeys}>
        <span id="org-chart-navigation" className="p-sr-only">
          Arrastra o usa las flechas para mover el organigrama. Usa la rueda o los controles para ajustar el zoom.
        </span>
        <div className="hr-org-chart__diagram" ref={diagramRef} style={camera ? {
          '--org-chart-scale': camera.scale,
          '--org-chart-x': `${camera.x}px`,
          '--org-chart-y': `${camera.y}px`,
        } : { visibility: 'hidden' }}>
          {charts}
        </div>
      </div>
      <div className="hr-org-chart__zoom" role="group" aria-label="Zoom del organigrama">
        <Button type="button" icon="pi pi-minus" text title="Alejar" aria-label="Alejar"
          disabled={!camera || camera.scale <= minZoom}
          onClick={() => setCamera((current) => zoomAt(current, 1 / zoomStep))} />
        <output aria-label="Nivel de zoom">{Math.round((camera?.scale ?? 1) * 100)}%</output>
        <Button type="button" icon="pi pi-plus" text title="Acercar" aria-label="Acercar"
          disabled={!camera || camera.scale >= maxZoom}
          onClick={() => setCamera((current) => zoomAt(current, zoomStep))} />
        <Button type="button" icon="pi pi-expand" text title="Ajustar a la vista" aria-label="Ajustar a la vista"
          onClick={() => setCamera(fitCamera(viewportRef.current, diagramRef.current))} />
      </div>
    </div>
  );
}

export default function OrganizationChartPage({ module }) {
  const [request, setRequest] = useState({ employeeId: null });
  const [result, setResult] = useState(null);
  const [options, setOptions] = useState([]);
  const { employeeId } = request;
  const { endpoint, title } = module;
  const loading = result?.request !== request;

  useEffect(() => {
    const controller = new AbortController();
    const params = request.employeeId === null ? undefined : { employeeid: request.employeeId };

    listRecords({ endpoint, params }, controller.signal)
      .then((roots) => {
        if (controller.signal.aborted) return;
        if (request.employeeId === null) setOptions(managerOptions(roots));
        setResult({ request, chart: roots.map((employee) => toChartNode(employee, true)) });
      })
      .catch((error) => {
        if (controller.signal.aborted) return;
        setResult({ request, error: apiErrorMessage(error, 'No se pudo cargar el organigrama.') });
      });

    return () => controller.abort();
  }, [endpoint, request]);

  return (
    <section className="crud-page hr-org-chart" aria-labelledby="org-chart-title">
      <h1 id="org-chart-title">{title}</h1>

      <div className="hr-org-chart__toolbar">
        <div className="hr-org-chart__filter">
          <label id="org-chart-employee-label" htmlFor="org-chart-employee">Ver equipo de</label>
          <Dropdown
            inputId="org-chart-employee"
            ariaLabelledBy="org-chart-employee-label"
            value={employeeId}
            options={options}
            onChange={(event) => setRequest({ employeeId: event.value ?? null })}
            placeholder="Toda la empresa"
            filter
            filterPlaceholder="Buscar por nombre o ID"
            showClear
            disabled={options.length === 0}
            panelClassName="crud-catalog-panel"
            emptyFilterMessage="No se encontraron supervisores."
          />
        </div>

        <Button type="button" icon="pi pi-refresh" outlined
          title="Actualizar organigrama" aria-label="Actualizar organigrama"
          disabled={loading} onClick={() => setRequest({ employeeId })} />
      </div>

      <div className="hr-org-chart__panel" aria-busy={loading}>
        {loading && (
          <div className="hr-org-chart__loading">
            <Skeleton width="11rem" height="5rem" />
            <span className="p-sr-only">Cargando organigrama…</span>
          </div>
        )}
        {!loading && result.error && <p role="alert">{result.error}</p>}
        {!loading && result.chart?.length === 0 && <p>No hay empleados para mostrar.</p>}
        {!loading && result.chart?.length > 0 && <OrganizationChartCanvas roots={result.chart} />}
      </div>
    </section>
  );
}
