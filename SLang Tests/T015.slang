a(): Integer is
    return 1
end

b: Integer is a()

if b == 0 then
    b := 18 - 3 + 6 / 2 * 11
else
    while b < 5 loop
        b := b + 1
    end
end

StandardIO.put(b)
