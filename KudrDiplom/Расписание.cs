//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KudrDiplom
{
    using System;
    using System.Collections.Generic;
    
    public partial class Расписание
    {
        public int ID_Расписания { get; set; }
        public int ID_Курса { get; set; }
        public Nullable<System.DateTime> Дата_начала { get; set; }
        public string День_недели { get; set; }
        public Nullable<System.TimeSpan> Время_начала { get; set; }
        public Nullable<System.TimeSpan> Время_окончания { get; set; }
    
        public virtual Курсы Курсы { get; set; }
    }
}
