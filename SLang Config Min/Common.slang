unit Integer is end
unit List[T] is end
unit String is end

unit A is

    very_important_variable: Integer is 42

end

unit B is

    just_copy_of_var: Integer is A.very_important_variable

    other_importnat_variable: Integer is 451

end

unit C is

    override routine main(args: List[String]): Integer is

		return A.very_important_variable + B.other_importnat_variable

	end

end