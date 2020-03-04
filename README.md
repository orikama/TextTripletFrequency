# TextTripletFrequency

Target Framework - .NET Core 3.1

Был взят общедоступный английский текст, и так как 8259 строк (435КБ) слишком скучно, текст был размножен до 3312260 строк (170МБ).

## 1. SimpleWorker 

### 1.1 Regex.Matches

Первый вариант с разбивкой строки на слова через Regex.Matches был слишком медленным. 24078мс На такую простую задачу и 50% времени уходит
на Regex.Matches. К тому же regex "\b([A-Z]{3,})\b" - еще и оказался некорректным.

![alt text](https://github.com/orikama/TextTripletFrequency/blob/master/Profiler/1.PNG "Regex.Matches")

### 1.2 Regex.Split

18668мс - Лучше, но все ещё медленно, 46% на Regex.Split.

![alt text](https://github.com/orikama/TextTripletFrequency/blob/master/Profiler/2.PNG "Regex.Split")

### 1.3 string.Split

11476мс - Гораздо лучше, разбивка на слова уходит на второе место, теперь лидирует Dictionary. Как решать такую проблему, я не придумал.
Так же данный метод может работать некорректно, так как не учитывает все символы, которые могут встретиться в тексте.

![alt text](https://github.com/orikama/TextTripletFrequency/blob/master/Profiler/3.PNG "string.Split")

## 2. Multithreading, ConcurrentDictionary

Раз уж задание было - обработка текста в многопоточном режиме, почему бы не обрабатывать строки в разных потоках.
Даже не смотря на то, что мой i3-6100 имеет 2 ядра, прирост был, но незначительный(ожидаемо) - 10320мс (NumberOfThreads = 2), 
позже я запросил помощь зала для теста многопоточности.

![alt text](https://github.com/orikama/TextTripletFrequency/blob/master/Profiler/4.PNG "MultithreadedWorker, ConcurrentDictionary")

Проблема Dictionary не решена, особенно учитывая то, что теперь это ConcurrentDictionary.

## 3. Multithreading, Multiple Dictionary

Вместо ConcurrentDictionary, каждый поток использует свой Dictionary, затем сливаем результаты.
7938мс (NumberOfThreads = 2).

![alt text](https://github.com/orikama/TextTripletFrequency/blob/master/Profiler/5.PNG "Multithreading, Multiple Dictionary")

## 4. Тесты на i5-3470, 4 ядра

Тут я вспомнил, что все это время тесты были при Debug конфигурации и пора переключаться на Release.

| Класс                             | Кол-во потоков | Время |
|:---------------------------------:|:--------------:|:-----:|
|MultithreadWorker                  | 2              | 9878мс |
|MultithreadWorker                  | 4              | 7847мс |
|MultithreadWorker_MultiDictionaries| 2              | 7761мс |
|MultithreadWorker_MultiDictionaries| 3              | 5940мс |
|MultithreadWorker_MultiDictionaries| 4              | 5599мс |

## Итоги

Куда рыть дальше я не знаю, наверняка здесь есть ещё возможности для оптимизации, особенно оптимизации по памяти
которым не было уделено внимания (особенно когда каждый поток имеет свой Dictionary). Возможно я что-то упускаю, и делаю в корне неправильно.
Возможно я слишком сильно угнался за быстродействием, забывая про корректность решения и качество кода.
Но это все что я смог придумать за пару дней, сроки поджимают.
