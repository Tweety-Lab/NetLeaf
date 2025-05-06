namespace CSharpTests
{
    public static class Methods
    {
        public static void TestStaticMethod()
        {
            // Write to a file
            File.WriteAllText("STATICVOIDMETHODSTEST.txt", "TestStaticMethod() Ran successfully!");
        }
    }
}
