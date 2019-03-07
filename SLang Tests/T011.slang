val unit Integer is
    init(): as this is
	   return 0;
	end
end

i:Integer is Integer(0)
j:Integer is Integer(1)

//while i < 10 loop // i in 1..10
   if i > 5 then
     j := j + 2
   else
     j := j - 4;
   end
//end

Test(other: Integer): Integer is
    return 2*other;
end //