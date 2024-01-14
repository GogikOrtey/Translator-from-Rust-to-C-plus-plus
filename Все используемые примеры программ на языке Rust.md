### Примеры программ на языке Rust:

Выводит Hello, world! в консоль

```rust
fn main() {
    println!("Hello, world!");
}

// Простой пример, который показывает что парсер может обрабатывать простейшие конструкции типа функций
```

Пример цикла с условием:

```rust
// !!! Исправить

while ((a / 2) < 30) {
  a += 10;
}

// Пример посложнее, который показывает, что парсер умеет обрабатывать комбинации из бинарных операций
```

Возводит число в 6ю степень, в цикле while. Результат выводит в консоль

```rust
// !!! Исправить

fn main() {
    let num = 2;
    let mut result = 1;
    let mut power = 6;

    while (power > 0) {
        result *= num;
        power -= 1;
    }

    println!("2 в степени 6 равно {}", result);
}
```

---

Проверка семантического анализатора:

Любая простая ошибка:

```rust
// Провека ошибки +++

fn main() {
    println!("Hello, world!");
    fn 123
}
```

Неинициализированная переменная:

```rust
// Провека ошибки +++

fn main() {
    let a = 2;
    let b = 1;

    c = a + b;

    println!("с = {}", с);
}
```

Неверный ввод:

```rust
// !!! Исправить

↨↑→←
```



---

#### Новые примеры кода:

```rust
// Провека ошибки +++

a = 5;
let mut a = 25;
```

```rust
// +++

let mut num = 2;
```

```rust
// +++

let mut num = 2;
num = 4;
```

```rust
// Провека ошибки +++

let mut num = 2;
num = 4;
nam = 4;
```

```rust
// +++

let mut num = 2;
num = 4;
println!("num = {}", num);
```

```rust
// +++

let mut length = 2;
length = 4;
let mut width = 8;
//let mut result = length*width;
let mut result = 0;
result = length*width;
//result = 4;


//println!("num = {}", num);
println!("length {} * width {} = result {}", length, width, result);

// nam = 2;

//fn main() {
//    println!("Hello, world!");
//}
```

```rust
// Провека ошибки +++

let mut a;
let mut b = 3;
```

```rust
// +++

let a = 2;
```

```rust
// +++

fn main() {
    let a: i8 = 0;
    let b: u8 = 0;
    let c: i16 = 0;
    let d: u16 = 0;
    let e: i32 = 0;
    let f: u32 = 0;
    let g: i64 = 0;
    let h: u64 = 0;
    let i: f32 = 0.0;
    let j: f64 = 0.0;
}
```

```rust
// +++

let mut a = 7;
let bool1: bool = false;
let char1: char = 'a';
let bool2 = false;
let char2 = 'a';
let str1 = "123";
// let mut str1 = "123";
```

```rust
// Провека ошибки +++

fn main() {
    let a: u16 = 0;
    
    let b: i128 = 0;
    let c: u128 = 0;
    let d: isize = 0;
    let e: usize = 0;
}
```

```rust
// +++

let a: i8 = 0;
let b: u16;
//let d: u size = 0;
```

```rust
// Провека ошибки +++

io::stdin().read_line(&mut input).expect("Ошибка чтения строки"); // Читаем строку из входного потока
```

```rust
// +++

let mut a = 25;
let mut b = 4;
b = b * b;
```

```rust
// Провека ошибки +++

let mut a = 25;
let b = 4;
b = b * b;
```

```rust
// Провека ошибки +++

let mut b = 4;
b = b * b;
let mut b = 4;
```

```rust
// +++

let mut a = 25;
```

---

Инициализация значения переменной float

```rust
// +++

let j: f64 = 10.1;
let mut f = 5.6;
```

Задание строк:

```rust
// +++

let mut str1 = "123";
let e: String = "Hello, Rust!";
str1 = e;
```

Проверка на полностью корректный код во входном файле:

```rust
// +++

fn main() {
    let a = 25;
}
```

Большой пример с объявлением переменных

```rust
// +++

fn main() {
    let mut a: i32 = 5; // int
    let b: f32 = 4.5; // float
    let c: bool = true; // bool
    let d: char = 'R'; // char
    let e: String = "Hello, Rust!"; // string

    // Неиспользуемая переменная
    let f: i32 = 10;
}
```

---

Проверка двойной операции

```rust
// +++

let mut a = 10;
a += 5;
```

```rust
// ---
let a = 25;
let j: f64 = 10.0;
let mut f = 5.6;

if(a >= 25)
{
    f = j*f;
} else {
    println!("Нет");
}

println!("f = {}", f);
```

---

Операции и и или

```rust
// +++

let a = 5;
let b = 10;
let c = 2;

let mut result1: u16;
let mut result2: u16;

// Без скобок, умножение выполняется первым
result1 = a + b * c; 
println!("Результат без скобок: {}", result1);

// Со скобками, сложение выполняется первым
result2 = (a + b) * c; 
println!("Результат со скобками: {}" result2);
```

```rust
// ---

let mut a = 5;
let b = 10;

// Логические операции
if a > 3 && b < 15 {
    println!("'a' больше 3 И 'b' меньше 15");
}

if a < 3 || b > 15 {
    println!("'a' меньше 3 ИЛИ 'b' больше 15");
}

// Двойные операции
a += b; 
println!("a += b: {}", a);

a *= b; 
println!("a *= b: {}", a);
```



---

### Примеры для проверки - сделаю их, и всё будет готово

Сделал дерево разбора, но не закончил кодогенератор:

```rust
// +++

fn main() {
    another_function(5);
}

fn another_function(x: i32) {
    println!("The value of x is: {}", x);
}
```



---

Полностью отлаженные примеры:

```rust
// +++

// Цикл while
let mut j = 0;
while (j < 5) {
    println!("Число: {}", j);
    j += 1;
}
```

Если не обернул аргумент в скобки, будет исключение

```rust
// +++

// if else и else if

let a = 10;

if (a < 5) {
    println!("a меньше 5");
} else if (a == 5) {
    println!("a равно 5");
} else {
    println!("a больше 5");
}
```

Ошибка:

```rust
// Провека ошибки +++

// if else и else if

let a = 10;

if (a < 5) {
    println!("a меньше 5");
} else if (a = 5) { // Ошибка тут
    println!("a равно 5");
} else {
    println!("a больше 5");
}
```



```rust
// +++

let mut a = 0;
// Цикл for
for i in 0..5 { // от 0 до 4
    println!("Число: {}", i);
}
```

























