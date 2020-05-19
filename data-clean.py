import sys


def load(day=1):
    """Load the raw HTML of day 1 to day 7 from raw-data/, strip all newlines
    and formatting, and splits the entire file into individual lines by '>'.
    Returns a list of strings, each element is an HTML tag, or content wrapped
    in HTML tags.

    Keyword arguments:
    day -- an integer indicating which day's data will be loaded (default 1)
    """

    data = ""
    with open(f'raw-data/day{day}.html') as fin:
        for line in fin:
            data += line.strip()
    lines = data.replace('<', '\n<').replace('>', '>\n').split('\n')
    return [line for line in lines if line.strip() != '']


def parse(data):
    """Extract word-definition pairs from the data provided by the load()
    function. Returns a dictionary of word-definition pairs.
    """
    res = {}
    while len(data) > 0:
        line = data.pop(0).strip()
        if line == '<div class="SetPageTerms-term">':
            word, defn = parse_item(data)
            res[word] = defn
        elif len(res) != 0 and line == '</section>':
            # This signals the end of all definitions
            break

    return res


def parse_item(data):
    """Helper function for parse() to extract one entry from data. Returns
    a tuple of word followed by its definition.
    """
    DEFINITION_TAG = '<span class="TermText notranslate lang-en">'

    # Keep popping until we reach the span containing the word
    word = ''
    while len(data) > 0:
        line = data.pop(0).strip()
        if line == DEFINITION_TAG:
            word = data.pop(0).strip()
            break

    # Keep popping until we reach the span containing definition
    definition = ''
    processed = False
    while len(data) > 0 and not processed:
        line = data.pop(0).strip()
        if line == DEFINITION_TAG:
            # Sometimes there will be <br> tags. We will replace
            # those by "\n" and add them to the definition.
            while True:
                line = data.pop(0)
                if line == '</span>':
                    break
                elif line == '<br>':
                    definition += '\n'
                else:
                    definition += line.strip()
            processed = True

    return (word, definition)


if __name__ == '__main__':
    if len(sys.argv) >= 2:
        day = int(sys.argv[1])
    else:
        day = 1
    data = load(day)
    res = parse(data)
    for k, v in res.items():
        print(k, v.replace('\n', '\\n'))
