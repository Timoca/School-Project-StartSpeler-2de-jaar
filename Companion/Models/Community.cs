﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.Models
{
	public class Community
	{
		[Key]
		public int Id { get; set; } = default!;

		public string Naam { get; set; } = default!;
		public List<Evenement> Evenementen { get; set; } = default!;
	}
}
