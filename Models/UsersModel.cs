using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50, ErrorMessage = "{1} {0} Karakter Fazla Olamaz")]
        public string Adi { get; set; }
        [MaxLength(50, ErrorMessage = "{1} {0} Karakter Fazla Olamaz")]

        public string Soyadi { get; set; }

        [MaxLength(30, ErrorMessage = "{1} {0} Karakter Fazla Olamaz")]
        public override string UserName { get => base.UserName; set => base.UserName = value; }

        [MaxLength(20)]
        public string Sifre { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [MaxLength(250, ErrorMessage = "{1} {0} Karakter Fazla Olamaz")]
        public string Avatar { get; set; }
        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; }
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        [NotMapped]
        public string AdSoyad { get { return $"{Adi} {Soyadi}"; } }
    }
        public class ApplicationRole : IdentityRole
        {
            public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
            public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
        }

        public class ApplicationUserRole : IdentityUserRole<string>
        {
            public virtual ApplicationUser User { get; set; }
            public virtual ApplicationRole Role { get; set; }
        }

        public class ApplicationUserClaim : IdentityUserClaim<string>
        {
            public virtual ApplicationUser User { get; set; }
        }

        public class ApplicationUserLogin : IdentityUserLogin<string>
        {
            public virtual ApplicationUser User { get; set; }
        }

        public class ApplicationRoleClaim : IdentityRoleClaim<string>
        {
            public virtual ApplicationRole Role { get; set; }
        }

        public class ApplicationUserToken : IdentityUserToken<string>
        {
            public virtual ApplicationUser User { get; set; }
        }

    }
