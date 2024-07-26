namespace Periods;

public class PeriodInfo
{
    public int Day { get; private init; }
    public string Description { get; private init; }

    public static PeriodInfo Create(DateOnly firstPeriodDate)
    {
        int periodDay = DateOnly.FromDateTime(DateTime.Today).DayNumber - firstPeriodDate.DayNumber + 1;
        return Create(periodDay);
    }
    
    public static PeriodInfo Create(int periodDay)
    {
        string description;
        if (Between(periodDay, 1, 2))
        {
            description =  
                "Начало цикла. Первый день месячных. Спазмы матки вызывают ноющую боль.\nНизкий уровень эстрогенов вызывает усталость.\nАне пора сделать лёгкую зарядку.";
        }

        else if (Between(periodDay, 3, 5))
        {
            description =
                "Месячные заканчиваются. Уровень эстрогена повышается. И вместе с ним -- Анины силы. Аня чувствует себя бодрее. У Ани время выложиться на все 100%.";
        }
        
        else if (Between(periodDay, 6, 9))
        {
            description =
                "Уровень эстрогена повышается. Лицо выглядит симметричнее. Кожа сияет. Уровень тестостерона повышается. Аня чувствует себя на высоте и проявляет находчивость. Для неё самое время проявить инициативу.";
        }

        else if (Between(periodDay, 10, 13))
        {
            description =
                "Эстроген достигает максимума. Аня на пике своей сексуальности и готова к зачатию. Оргазмы яркие и их легко достичь. Аня полна оптимизма и легко идёт на контакт.";
        }

        else if (Between(periodDay, 14, 14))
        {
            description = "Овуляция. Яйцеклетка выходит из яичников. Неоплодотворённая яйцеклетка разрушается.";
        }

        else if (Between(periodDay, 15, 18))
        {
            description = "Гормональный всплеск может обострить эмоции и вызвать усталость.";
        }
         
        else if (Between(periodDay, 19, 22))
        {
            description = "Уровень эстрогена падает. Уровень тестостерона и прогестерона растёт. Активная выработка кожного сала ведёт к высыпаниям и повышенной чувствительности кожи. Время позаботиться о правильном питании.";
        }
        
        else if (Between(periodDay, 23, 25))
        {
            description = "Повышение прогестерона может вызвать отёки и снизить либидо. Ничего не хочется делать.";
        }
        
        else if (Between(periodDay, 26, 31))
        {
            description =
                "Несмотря на апатию и леность можно справиться с ПМС и поднять серотонин с помощью спорта! Чтобы справиться с чувствительностью груди, Ане нужно употреблять меньше кофеина (ты попытался), есть меньше сладкого, пить много воды.";
        }
        else
        {
            throw new ArgumentOutOfRangeException($"period day is equal to {periodDay}");
        }

        return new PeriodInfo() { Day = periodDay, Description = description };

        bool Between(int number, int leftIncluding, int rightIncluding)
        {
            return leftIncluding <= number && number <= rightIncluding;
        }
    }
}