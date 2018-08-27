//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlinePermissionSlips.Models.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Student
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Student()
        {
            this.GuardianApprovals = new HashSet<GuardianApproval>();
            this.Guardians = new HashSet<AspNetUser>();
            this.GuardianTempEmails = new HashSet<GuardianTempEmail>();
        }
    
        public int ID { get; set; }
        public string FullName { get; set; }
        public int ClassRoomID { get; set; }
        public int SchoolID { get; set; }
        public int StudentNumber { get; set; }
        public string Guardian1TempEmail { get; set; }
        public string Guardian2TempEmail { get; set; }
    
        public virtual ClassRoom ClassRoom { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GuardianApproval> GuardianApprovals { get; set; }
        public virtual School School { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUser> Guardians { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GuardianTempEmail> GuardianTempEmails { get; set; }
    }
}
