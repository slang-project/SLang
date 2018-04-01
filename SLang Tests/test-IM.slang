unit Integer is
end

i is Integer(0) // i: is Integer(0) - проходит? Проходит, и результат ноды в дереве 
                // показан на картинке isInteger_WITH_colon - там вызов Call стоит на одном уровне с объявлением переменной
                // если не ставить двоеточие - 

while i < 10 loop
 if i < 5 then
  i := i + 2
 else
  i := i + 1
 end
end

routine Test(other: Integer): Integer is
    return 2*other;
end //