using System;
using SQLite;

namespace GREVocab {
    /*
     * Class representing a row of record in the database.
     * Holds an Json string that will expand to a Word object.
     */
    [Table("record")]
    public class Record {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Json { get; set; }
        public DateTime LastMemorized { get; set; }
    }
}
