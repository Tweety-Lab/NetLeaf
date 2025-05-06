namespace CSharpTests
{
    public static class Methods
    {
        public static void TestStaticMethod()
        {
            // Write to a file
            File.WriteAllText("STATICVOIDMETHODSTEST.txt", "TestStaticMethod() Ran successfully!");
        }

        public static int TestStaticIntMethod()
        {
            return 32;
        }

        public static uint TestStaticUIntMethod()
        {
            return 32;
        }

        public static string TestStaticStringMethod()
        {
            return "ABC123";
        }
    }
}
