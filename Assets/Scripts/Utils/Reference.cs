using System;

namespace Utils {
    public class Reference<T> {
        private readonly Func<T> _getter;
        public Reference(Func<T> getter) {
            _getter = getter;
        }
        public T Value => _getter();
    }
}