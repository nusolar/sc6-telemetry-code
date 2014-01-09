"""python __builtins__ reference
Magic method names are given in comments"""

"""types:"""
bool
int, float, complex # __int__, __float__
list, tuple
str # __str__
bytes, bytearray, memoryview
set, frozenset
dict
slice
object
type
super

"""type conversion:"""
repr, ascii # __repr__
bin, hex, oct # __index__
chr, ord
format # __format__
hash
id

"""math:"""
abs, divmod, pow
round # __round__
max, min, sum

"""collection utilities:"""
len # __len__
+ iter # __iter__
any, all, next # __next__
+ enumerate, filter, map, range, reversed, sorted, zip # __reversed__

"""introspection:"""
dir # __dir__
globals, locals, vars # __dict__
callable, isinstance, issubclass # __call__
compile, exec, eval

"""function descriptors:"""
+ classmethod, staticmethod, property

"""attrs:"""
delattr, getattr, hasattr, setattr # __getattr__, __setattr__

"""io:"""
print
input
open

"""interactive:"""
help
exit, quit, copyright, credits
