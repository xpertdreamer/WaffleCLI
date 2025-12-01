#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <algorithm>

// Simple calculator that demonstrates console I/O
class SimpleCalculator {
public:
    void Run() {
        PrintHeader();
        
        std::string input;
        while (true) {
            std::cout << "calc> ";
            std::getline(std::cin, input);
            
            if (input == "exit" || input == "quit") {
                std::cout << "Goodbye from C++ calculator!" << std::endl;
                break;
            }
            
            ProcessCommand(input);
        }
    }
    
private:
    void PrintHeader() {
        std::cout << "===================================" << std::endl;
        std::cout << "    Simple C++ Calculator v1.0     " << std::endl;
        std::cout << "===================================" << std::endl;
        std::cout << "Commands:" << std::endl;
        std::cout << "  add <num1> <num2>  - Addition" << std::endl;
        std::cout << "  sub <num1> <num2>  - Subtraction" << std::endl;
        std::cout << "  mul <num1> <num2>  - Multiplication" << std::endl;
        std::cout << "  div <num1> <num2>  - Division" << std::endl;
        std::cout << "  fib <n>           - Fibonacci sequence" << std::endl;
        std::cout << "  echo <text>       - Echo input" << std::endl;
        std::cout << "  help              - Show this help" << std::endl;
        std::cout << "  exit              - Exit program" << std::endl;
        std::cout << "===================================" << std::endl;
    }
    
    void ProcessCommand(const std::string& cmd) {
        std::istringstream iss(cmd);
        std::string operation;
        iss >> operation;
        
        if (operation == "add") {
            double a, b;
            if (iss >> a >> b) {
                std::cout << "Result: " << (a + b) << std::endl;
            } else {
                std::cout << "Error: Need two numbers" << std::endl;
            }
        }
        else if (operation == "sub") {
            double a, b;
            if (iss >> a >> b) {
                std::cout << "Result: " << (a - b) << std::endl;
            } else {
                std::cout << "Error: Need two numbers" << std::endl;
            }
        }
        else if (operation == "mul") {
            double a, b;
            if (iss >> a >> b) {
                std::cout << "Result: " << (a * b) << std::endl;
            } else {
                std::cout << "Error: Need two numbers" << std::endl;
            }
        }
        else if (operation == "div") {
            double a, b;
            if (iss >> a >> b) {
                if (b != 0) {
                    std::cout << "Result: " << (a / b) << std::endl;
                } else {
                    std::cout << "Error: Division by zero!" << std::endl;
                }
            } else {
                std::cout << "Error: Need two numbers" << std::endl;
            }
        }
        else if (operation == "fib") {
            int n;
            if (iss >> n && n >= 0) {
                std::cout << "Fibonacci sequence: ";
                for (int i = 0; i <= n; i++) {
                    std::cout << Fibonacci(i) << " ";
                }
                std::cout << std::endl;
            } else {
                std::cout << "Error: Need positive integer" << std::endl;
            }
        }
        else if (operation == "echo") {
            std::string text;
            std::getline(iss, text);
            if (!text.empty()) {
                std::cout << "Echo: " << text << std::endl;
            } else {
                std::cout << "Error: Need text to echo" << std::endl;
            }
        }
        else if (operation == "help") {
            PrintHeader();
        }
        else if (!operation.empty()) {
            std::cout << "Unknown command: " << operation << std::endl;
            std::cout << "Type 'help' for available commands" << std::endl;
        }
    }
    
    long long Fibonacci(int n) {
        if (n <= 1) return n;
        long long a = 0, b = 1;
        for (int i = 2; i <= n; i++) {
            long long temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
};

int main() {
    SimpleCalculator calculator;
    calculator.Run();
    return 0;
}