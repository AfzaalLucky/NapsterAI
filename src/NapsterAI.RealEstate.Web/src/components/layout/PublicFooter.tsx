export function PublicFooter() {
  return (
    <footer className="border-t">
      <div className="mx-auto flex max-w-7xl flex-col gap-2 px-4 py-8 text-sm text-muted-foreground sm:px-6 lg:px-8">
        <p>&copy; {new Date().getFullYear()} NapsterAI Real Estate. All rights reserved.</p>
        <p>Listings and pricing shown are for demonstration purposes.</p>
      </div>
    </footer>
  );
}
