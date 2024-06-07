namespace SignalR_Demo_Application.Collections
{
    public class LinkedList<T>
    {
        public class Node<J>
        {
            public J? Value { get; set; }
            public Node<J>? Next { get; set; }
            public override string ToString()
            {
                return $"value: {Value} and next: {Next.Value}";
            }
        }
        public Node<T> Head {  get; set; }
        public Node<T> Tail { get; set; }
        public int Length { get; set; }
        public LinkedList() { }
        public LinkedList(T Value) {
        if(Value == null)
            {
                Console.Write("Value can't be empty");
            }
        }
    }
}
