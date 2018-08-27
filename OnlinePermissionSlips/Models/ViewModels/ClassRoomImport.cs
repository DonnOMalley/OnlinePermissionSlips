using System.Collections.Generic;

namespace OnlinePermissionSlips.Models.ViewModels
{
	public class ClassRoomImport
	{
		public int ID { get; set; }
		public int SchoolID { get; set; }
		public string SchoolName { get; set; }
		public string RoomNumber { get; set; }
		public string TeacherName { get; set; }

		public List<StudentImport> Students { get; set; }

		public ClassRoomImport()
		{
			Students = new List<StudentImport>();
		}

		public ClassRoomImport(int ID, int SchoolID, string SchoolName, string TeacherName, string RoomNumber)
		{
			Students = new List<StudentImport>();

			this.ID = ID;
			this.SchoolID = SchoolID;
			this.SchoolName = SchoolName;
			this.TeacherName = TeacherName;
			this.RoomNumber = RoomNumber;
		}
	}
}