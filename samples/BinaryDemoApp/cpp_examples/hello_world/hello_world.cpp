#include <iostream>
#include <string>

int main() {
    std::cout << "===================================" << std::endl;
    std::cout << "      C++ Hello World v1.0         " << std::endl;
    std::cout << "===================================" << std::endl;
    
    std::string name;
    std::cout << "Enter your name: ";
    std::getline(std::cin, name);
    
    if (name.empty()) {
        name = "World";
    }
    
    std::cout << "Hello, " << name << "!" << std::endl;
    std::cout << "This is a C++ program running inside WaffleCLI TUI." << std::endl;
    std::cout << "Press Enter to exit..." << std::endl;
    
    std::cin.get();
    return 0;
}