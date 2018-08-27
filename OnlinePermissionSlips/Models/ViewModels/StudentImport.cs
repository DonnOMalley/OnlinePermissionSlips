namespace OnlinePermissionSlips.Models.ViewModels
{
	public class StudentImport
	{
		public int ID { get; set; }
		public int StudentNumber { get; set; }
		public string FullName { get; set; }
		public string Guardian1TempEmail { get; set; }
		public string Guardian2TempEmail { get; set; }
	}
}