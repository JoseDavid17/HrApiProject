import { useEffect, useState } from 'react';
import { listRecords } from './hrApi';
import { hrModules } from './hrModules';
import './HrHomePage.css';

const numberFormat = new Intl.NumberFormat('es-HN');
const percentFormat = new Intl.NumberFormat('es-HN', { style: 'percent', maximumFractionDigits: 1 });

function countWithoutManager(departments) {
  return departments.filter(({ managerId }) => managerId == null).length;
}

const indicators = [
  {
    path: '/empleados', label: 'Empleados', icon: 'users',
    detail: (records) => `${numberFormat.format(records.filter(({ departmentName }) => !departmentName?.trim()).length)} sin departamento`,
  },
  {
    path: '/departamentos', label: 'Departamentos', icon: 'building',
    detail: (records) => `${numberFormat.format(countWithoutManager(records))} sin responsable`,
  },
  {
    path: '/puestos', label: 'Puestos', icon: 'briefcase',
    detail: (records) => {
      const withRange = records.filter(({ minSalary, maxSalary }) => minSalary != null && maxSalary != null && minSalary <= maxSalary);
      return `${numberFormat.format(withRange.length)} con rango salarial`;
    },
  },
  {
    path: '/locaciones', label: 'Locaciones', icon: 'map-marker',
    detail: (records) => {
      const countries = new Set(records.map(({ countryId }) => countryId).filter(Boolean)).size;
      return countries === 1 ? 'En 1 país' : `En ${numberFormat.format(countries)} países`;
    },
  },
];

const shortcuts = [
  { path: '/empleados/organigrama', icon: 'sitemap', label: 'Organigrama' },
  { path: '/empleados/supervisores', icon: 'id-card', label: 'Supervisores' },
  { path: '/empleados/historial', icon: 'history', label: 'Historial laboral' },
];

function getDepartmentDistribution(employees) {
  const counts = new Map();
  for (const employee of employees) {
    const department = employee.departmentName?.trim() || null;
    counts.set(department, (counts.get(department) ?? 0) + 1);
  }

  const unassigned = counts.get(null) ?? 0;
  counts.delete(null);
  const departments = Array.from(counts).sort(([nameA, countA], [nameB, countB]) =>
    countB - countA || nameA.localeCompare(nameB, 'es'),
  );
  const shown = departments.slice(0, 5).map(([label, count]) => ({ key: `department-${label}`, label, count }));
  if (departments.length > 5) {
    const count = departments.slice(5).reduce((total, [, count]) => total + count, 0);
    shown.push({ key: 'other', label: 'Otros departamentos', count });
  }
  if (unassigned) shown.push({ key: 'unassigned', label: 'Sin departamento', count: unassigned });
  return shown;
}

function getRecentHires(employees) {
  return employees
    .map((employee) => ({ ...employee, date: /^(\d{4}-\d{2}-\d{2})(?:T|$)/.exec(employee.hireDate)?.[1] }))
    .filter(({ date }) => date)
    .sort((a, b) => b.date.localeCompare(a.date) || a.fullName.localeCompare(b.fullName, 'es'))
    .slice(0, 5);
}

function IndicatorCard({ indicator: { path, label, icon, detail }, result, loading }) {
  const records = result?.records;
  return (
    <a className="hr-home-kpi" href={`#${path}`}>
      <span className="hr-home-kpi-label">{label}</span>
      <span className="hr-home-kpi-icon"><i className={`pi pi-${icon}`} aria-hidden="true" /></span>
      <strong className="hr-home-kpi-value">{records ? numberFormat.format(records.length) : '—'}</strong>
      <span className="hr-home-kpi-detail">
        {records ? detail(records) : loading ? 'Cargando…' : 'Sin datos'}
      </span>
    </a>
  );
}

function PanelMessage({ loading, failed, empty, unavailable = 'Datos de empleados no disponibles.' }) {
  return <p className="hr-home-empty">{loading ? 'Cargando…' : failed ? unavailable : empty}</p>;
}

function DepartmentChart({ employees }) {
  const distribution = getDepartmentDistribution(employees);
  const highest = Math.max(...distribution.map(({ count }) => count));
  const stepSize = Math.max(1, highest / 4);
  const magnitude = 10 ** Math.floor(Math.log10(stepSize));
  const step = [1, 2, 2.5, 5, 10].map((factor) => factor * magnitude)
    .find((candidate) => Number.isInteger(candidate) && candidate >= stepSize);
  const ticks = [4, 3, 2, 1, 0].map((level) => level * step);
  const [maximum] = ticks;

  return (
    <div className="hr-home-chart">
      <div className="hr-home-chart-axis" aria-hidden="true">
        {ticks.map((tick) => <span key={tick}>{numberFormat.format(tick)}</span>)}
      </div>
      <ul className="hr-home-chart-bars" aria-label="Cantidad de empleados por departamento">
        {distribution.map(({ key, label, count }) => (
          <li key={key} data-group={key === 'other' || key === 'unassigned' ? 'summary' : undefined}>
            <span className="hr-home-chart-name">{label}</span>
            <div className="hr-home-chart-column" style={{ '--bar-size': `${count / maximum * 100}%` }}>
              <span className="hr-home-chart-fill" aria-hidden="true" />
              <strong className="hr-home-chart-count">
                {numberFormat.format(count)}
                <span className="hr-home-sr-only"> empleados ({percentFormat.format(count / employees.length)})</span>
              </strong>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}

function DepartmentPanel({ result, loading }) {
  const employees = result?.records;

  return (
    <section className="hr-home-panel hr-home-chart-panel" aria-labelledby="hr-home-distribution" aria-busy={loading}>
      <header className="hr-home-panel-header">
        <h2 id="hr-home-distribution">Empleados por departamento</h2>
        {employees?.length > 0 && (
          <span className="hr-home-chart-legend">{numberFormat.format(employees.length)} en total</span>
        )}
      </header>
      {employees?.length ? (
        <DepartmentChart employees={employees} />
      ) : <PanelMessage loading={loading} failed={result?.failed} empty="Todavía no hay empleados registrados." />}
    </section>
  );
}

function DepartmentCoverage({ result, loading }) {
  const departments = result?.records;
  const total = departments?.length ?? 0;
  const unassigned = departments ? countWithoutManager(departments) : 0;
  const assigned = total - unassigned;
  const share = total ? assigned / total : 0;

  return (
    <section className="hr-home-panel hr-home-coverage" aria-labelledby="hr-home-coverage">
      <header className="hr-home-panel-header">
        <h2 id="hr-home-coverage">Responsables de departamento</h2>
      </header>
      {total > 0 ? (
        <>
          <div className="hr-home-coverage-value">
            <strong>{percentFormat.format(share)}</strong>
            <span>de departamentos con responsable</span>
          </div>
          <div className="hr-home-coverage-track" aria-hidden="true">
            <span style={{ width: `${share * 100}%` }} />
          </div>
          <dl className="hr-home-coverage-breakdown">
            <div><dt>Con responsable</dt><dd>{numberFormat.format(assigned)}</dd></div>
            <div><dt>Sin responsable</dt><dd>{numberFormat.format(unassigned)}</dd></div>
          </dl>
        </>
      ) : (
        <PanelMessage loading={loading} failed={result?.failed}
          empty="Todavía no hay departamentos registrados." unavailable="Datos de departamentos no disponibles." />
      )}
      <nav className="hr-home-shortcuts" aria-label="Accesos rápidos">
        {shortcuts.map(({ path, icon, label }) => (
          <a key={path} href={`#${path}`}>
            <i className={`pi pi-${icon}`} aria-hidden="true" />{label}
          </a>
        ))}
      </nav>
    </section>
  );
}

function RecentHires({ result, loading }) {
  const recent = getRecentHires(result?.records ?? []);

  return (
    <section className="hr-home-panel hr-home-hires" aria-labelledby="hr-home-hires" aria-busy={loading}>
      <header className="hr-home-panel-header">
        <h2 id="hr-home-hires">Últimas contrataciones</h2>
        <a className="hr-home-link" href="#/empleados">Ver empleados <i className="pi pi-chevron-right" aria-hidden="true" /></a>
      </header>
      {recent.length ? (
        <div className="hr-home-table-wrap" role="region" aria-labelledby="hr-home-hires" tabIndex={0}>
          <table className="hr-home-table" aria-labelledby="hr-home-hires">
            <thead>
              <tr>
                <th scope="col">Empleado</th>
                <th scope="col" className="hr-home-table-desktop">Puesto</th>
                <th scope="col" className="hr-home-table-desktop">Departamento</th>
                <th scope="col">Fecha de ingreso</th>
              </tr>
            </thead>
            <tbody>
              {recent.map(({ id, fullName, jobTitle, departmentName, date }) => (
                <tr key={id}>
                  <th scope="row">
                    <span className="hr-home-person">
                      <span className="hr-home-initials" aria-hidden="true">
                        {fullName.trim().split(/\s+/).slice(0, 2).map((part) => part[0]).join('').toLocaleUpperCase('es-HN')}
                      </span>
                      <span>
                        {fullName}
                        <small className="hr-home-table-mobile">
                          {jobTitle && `${jobTitle} · `}{departmentName || 'Sin departamento'}
                        </small>
                      </span>
                    </span>
                  </th>
                  <td className="hr-home-table-desktop">{jobTitle || '–'}</td>
                  <td className="hr-home-table-desktop">{departmentName || 'Sin departamento'}</td>
                  <td><time dateTime={date}>{date.split('-').reverse().join('/')}</time></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : <PanelMessage loading={loading} failed={result?.failed} empty="Todavía no hay fechas de ingreso registradas." />}
    </section>
  );
}

export default function HrHomePage() {
  const [results, setResults] = useState(null);
  const loading = results === null;
  const employeeResult = results?.['/empleados'];
  const failures = results ? Object.values(results).filter(({ failed }) => failed).length : 0;

  useEffect(() => {
    const controller = new AbortController();

    Promise.allSettled(indicators.map(({ path }) => listRecords(hrModules[path], controller.signal)))
      .then((responses) => {
        if (controller.signal.aborted) return;
        const results = Object.fromEntries(responses.map((response, index) => [
          indicators[index].path,
          response.status === 'fulfilled' ? { records: response.value } : { failed: true },
        ]));
        setResults(results);
      });

    return () => controller.abort();
  }, []);

  return (
    <div className="hr-home">
      <header className="hr-home-heading">
        <h1>Resumen de RR. HH.</h1>
      </header>
      <div role="status">
        {!loading && failures > 0 && (
          <p className="hr-home-notice">
            <i className="pi pi-exclamation-circle" aria-hidden="true" />
            {failures === indicators.length ? 'No se pudo cargar el panel.' : 'Algunos indicadores no se pudieron cargar.'}
          </p>
        )}
      </div>
      <div className="hr-home-kpis" aria-label="Indicadores de recursos humanos" aria-busy={loading}>
        {indicators.map((indicator) => (
          <IndicatorCard key={indicator.path} indicator={indicator} result={results?.[indicator.path]} loading={loading} />
        ))}
      </div>
      <DepartmentPanel result={employeeResult} loading={loading} />
      <div className="hr-home-overview">
        <RecentHires result={employeeResult} loading={loading} />
        <DepartmentCoverage result={results?.['/departamentos']} loading={loading} />
      </div>
    </div>
  );
}
