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
    
    public partial class Итоговые_Работы
    {
        public int ID_Работы { get; set; }
        public int ID_Учащегося { get; set; }
        public int ID_Курса { get; set; }
        public string Название { get; set; }
        public string Описание { get; set; }
        public Nullable<int> Оценка { get; set; }
    
        public virtual Курсы Курсы { get; set; }
        public virtual Учащиеся Учащиеся { get; set; }
    }
}
