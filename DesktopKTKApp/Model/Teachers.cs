//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DesktopKTKApp.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Teachers
    {
        public int TeacherID { get; set; }
        public int UserID { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
