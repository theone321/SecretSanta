using System.Collections.Generic;

namespace SecretSanta.Models.SuperAdmin {
  public class SuperAdminPageModel {
    public int UserId { get; set; }
    public string UserName { get; set; }

    public List<SuperAdminUserSettings> UserList { get; set; } = new List<SuperAdminUserSettings>();

    public class SuperAdminUserSettings {
      /// <summary>
      /// The User's Id
      /// </summary>
      public int UserId { get; set; }
      /// <summary>
      /// The User's name
      /// </summary>
      public string Name { get; set; }
      /// <summary>
      /// The User's user name
      /// </summary>
      public string UserName { get; set; }
      /// <summary>
      /// Is this user a Super Admin?
      /// </summary>
      public bool IsSuperAdmin { get; set; }
    }
  }
}
