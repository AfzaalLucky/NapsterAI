import { Helmet } from "react-helmet-async";
import { InquiryForm } from "@/components/property/InquiryForm";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export function ContactPage() {
  return (
    <div className="mx-auto max-w-xl px-4 py-10 sm:px-6 lg:px-8">
      <Helmet>
        <title>Contact Us | NapsterAI Real Estate</title>
        <meta name="description" content="Get in touch with our team about any listing or general inquiry." />
      </Helmet>

      <h1 className="text-2xl font-semibold">Contact Us</h1>
      <p className="text-muted-foreground mt-2">
        Have a question about a listing? Reach out and one of our agents will get back to you.
      </p>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Send us a message</CardTitle>
        </CardHeader>
        <CardContent>
          <InquiryForm />
        </CardContent>
      </Card>
    </div>
  );
}
