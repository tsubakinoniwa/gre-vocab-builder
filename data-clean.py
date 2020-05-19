import sys
import re
import json


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
    res = []
    while len(data) > 0:
        line = data.pop(0).strip()
        if line == '<div class="SetPageTerms-term">':
            res.append(parse_item(data))
        elif len(res) != 0 and line == '</section>':
            # This signals the end of all definitions
            break

    return res


def parse_item(data):
    """Helper function for parse() to extract one entry from data. Returns
    a word dictionary built by build_world().
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

    return build_word(word, definition)


def build_word(word, definition):
    """From the word and definition of a word, build a dictionary represnting
    a word object, which will be saved as a JSON file. The structure of one
    word will be as follows:
    {
        'word': '...',
        'definitions': [
            {
                'part_of_speech': 'adj.',
                'definition_EN': '...',
                'definition_CN': '...',
                'synonym': ['...', '...']
            },
            ...
        ],
        'gre_synonym': ['...', '...']
    }
    'gre_synonym' are the synonyms commonly tested in the GRE.
    """

    # Remove quotes, insert spaces after commas, and insert spaces between
    # Chinese and English characters.
    definition = re.sub('&quot;\s*', '', definition)
    definition = re.sub(r'([\u4e00-\u9fff，（）]+)', r'\1 ', definition)
    definition = re.sub(',\s*', ', ', definition)
    parts = definition.replace('(', '\n(').split('\n')
    parts = list(map(lambda s: re.sub('\([0-9]*\)\s*', '', s), parts))
    parts = [part.strip() for part in parts if part.strip() != '']

    # At this stage, each element in parts will be a definition
    # or a gre_synonym. First parse all the definitions
    definitions = []
    for ele in parts:
        # Check if this is a gre_synonym. This will always occur at the end
        if ele.find('六选二同义词') != -1:
            continue

        ele_parts = ele.split(' ')
        obj = {
            'part_of_speech': ele_parts[0],
            'definition_EN': '',
            'definition_CN': '',
            'synonym': []
        }

        ind = 1
        # Extract English definition (definition_EN)
        while ind < len(ele_parts):
            part = ele_parts[ind]
            if len(re.findall(r'[\u4e00-\u9fff，（）]+', part)) == 0:
                obj['definition_EN'] += ' ' + part
            else:
                break
            ind += 1
        obj['definition_EN'] = obj['definition_EN'].strip()

        # Extract Chinese definition (definition_CN)
        while ind < len(ele_parts):
            part = ele_parts[ind]
            if len(re.findall(r'[\u4e00-\u9fff，（）]+', part)) != 0:
                obj['definition_CN'] += ' ' + part
            else:
                break
            ind += 1
        obj['definition_CN'] = obj['definition_CN'].strip()

        # Get synonynms if there is any
        while ind < len(ele_parts):
            syn = ele_parts[ind].replace(',', '').strip()
            if syn != '':
                obj['synonym'].append(syn)
            ind += 1

        # Store the object
        definitions.append(obj)

    # Parse gre_synonym if there is any
    gre_synonym = []
    if parts[-1].find('六选二同义词') != -1:
        syn_string = parts[-1].replace('六选二同义词：', '')
        syn_parts = syn_string.split(',')
        for part in syn_parts:
            if part.strip() != '':
                gre_synonym.append(part.replace(',', '').strip())

    return {
        'word': word,
        'definitions': definitions,
        'gre_synonym': gre_synonym
    }


if __name__ == '__main__':
    # Loads, parses, and saves data under data/ as a JSON file
    data = []
    for day in range(1, 8):
        data.extend(parse(load(day)))

    with open(f'data.json', 'w') as fout:
        json.dump(data, fout, indent=4, ensure_ascii=False)
