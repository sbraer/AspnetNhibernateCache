namespace Entities
{
	public class Author
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Sex Sex { get; set; }
	}

	public enum Sex : byte { M = 1, F = 2}
}
