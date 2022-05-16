# Расчёт доходов для декларации 3-НДФЛ по отчёту брокера eToro

Это приложение рассчитает прибыль, убытки и предложит, как заполнить декларацию. Для работы требуется [.NET Core 6 Runtime](https://dotnet.microsoft.com/en-us/download).

# Как пользоваться

1. Скачать отчёт брокера за весь год в формате xlsx.
2. Сохранить листы "Закрытые позиции" и "Дивиденды" в CSV с заголовками. Если пользуетесь Numbers, нужно сделать экспорт в CSV, выбрав "Сохранить каждую таблицу в отдельный файл".
3. Скачать с сайта ЦБ РФ [отчёт](https://www.cbr.ru/currency_base/dynamics/) в формате CSV о курсах валют за год. Важно: захватите последнюю неделю декабря предыдущего года, т.к. курс на новогодние праздники устанавливается на последней неделе декабря.
4. Перейти в директорию `EtoroTaxes`
5. Запустить приложение: `dotnet run -- -c <path_to_closed_positions_csv> -d <path_to_dividend_sheet_csv> -r <path_to_CBR_report> --date-from 2020.01.01 --date-to 2020.12.31`

## Нюанс

На момент публикации в ЛК налоговой какой-то баг и она не включает в декларацию то, что вы заполняете по иностранным источникам дохода. Рекомендую пока что заполнять в desktop-версии приложения "Декларация". Нужно [скачать](https://www.gnivc.ru/software/fnspo/ndfl_3_4/) программу для того года, за который вы подаёте декларацию. Вычет 209 не поддерживается в desktop-приложении, поэтому нужно в вычет с кодом 207 писать всю сумму убытка, без разбивки, предлагаемой этим приложением, даже если эта сумма больше дохода по коду 1535.

# Пояснительная записка

К декларации нужно приложить отчёт брокера в формате pdf и пояснительную записку. [Пример записки](Пояснительная%20записка.pdf).

# Полезные ссылки

- https://longterminvestments.ru/3-ndfl-guide
- https://journal.tinkoff.ru/invest-ndfl-optimization/
- https://shuchkin.ru/2021/06/etoro-3-ndfl/

# Донат

Я потратил много времени на изучение законодательства и методов расчёта. Если это приложение было вам полезно и вы хотите поддержать меня, велкам.

- BTC: `bc1q4twlzcjdypgfd3xu07wctrvka7uk7xws0m7ltg`
- ETH: `0xA4A14c71Dc16c8cEafF299d323c3a104303E4DAD`
