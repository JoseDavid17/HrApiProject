const currentYear = new Date().getFullYear();

export default function HrFooter() {
  return (
    <footer className="hr-footer">
      ® {currentYear} Jose David Alvarado · Proyecto portafolio
    </footer>
  );
}
