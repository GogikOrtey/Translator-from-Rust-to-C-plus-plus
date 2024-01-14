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
while ((a / 2) < 30) {
  a += 10;
}

// Пример посложнее, который показывает, что парсер умеет обрабатывать комбинации из бинарных операций
```

Возводит число в 6ю степень, в цикле while. Результат выводит в консоль

```rust
fn main() {
    let mut num = 2;
    let mut result = 1;
    let power = 6;

    while (power > 0) {
        result *= num;
        power -= 1;
    }

    println!("2 в степени 6 равно {}", result);
}
```

Просит пользователя ввести своё имя, а затем выводит сообщение: Привет, [введённое имя]

```rust
use std::io;

fn main() {
    println!("Введите ваше имя:");
    
    let mut name = String::new();
    io::stdin().read_line(&mut name)
        .expect("Не удалось прочитать строку");
    
    println!("Привет, {}", name.trim());
}
```

Просит ввести 2 числа, а затем выводит их сумму в консоль

```rust
use std::io;

fn main() {
    let mut first_num = String::new();
    let mut second_num = String::new();

    println!("Введите первое число:");
    io::stdin().read_line(&mut first_num).unwrap();

    println!("Введите второе число:");
    io::stdin().read_line(&mut second_num).unwrap();

    let sum = first_num.trim().parse::<i32>().unwrap() + second_num.trim().parse::<i32>().unwrap();

    println!("Сумма двух чисел: {}", sum);
}
```

