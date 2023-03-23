using System;

namespace Utils {
    public class Reference<T> {
        private readonly Func<T> _getter;
        private readonly Action<T> _setter;
        public Reference(Func<T> getter, Action<T> setter = null) {
            _getter = getter;
            _setter = setter;
        }
        public T Value {
            get { return _getter(); }
            set {
                if (_setter != null) _setter(value);
            }
        }
    }
}