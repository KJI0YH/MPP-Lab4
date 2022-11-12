namespace Core
{
    public class TestInfo
    {
        public string Name { get; }
        public string Content { get; }

        public TestInfo(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
