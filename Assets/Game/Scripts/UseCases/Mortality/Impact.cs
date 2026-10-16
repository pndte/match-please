namespace Bw.UseCases.Mortality
{
    public readonly struct Impact
    {
        public readonly Hit Hit;
        public readonly Contact Contact;

        public Impact(Hit hit, Contact contact)
        {
            Hit = hit;
            Contact = contact;
        }
    }
}
